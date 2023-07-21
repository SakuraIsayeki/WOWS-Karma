import base64
import binascii
import random
import tempfile
import os

import pathlib
from asyncio.log import logger

from fastapi import APIRouter, HTTPException, status, UploadFile, BackgroundTasks
from fastapi.responses import FileResponse
from renderer.data import ReplayData
from renderer.render import Renderer
from replay_parser import ReplayParser

from ..config import settings
from ..security import AuthenticatedUser

router = APIRouter()




@router.post("/render", dependencies=[AuthenticatedUser])
async def render_replay(
        file: UploadFile,
        background_tasks: BackgroundTasks,
        replay_id: str | None = None,
        target_player_id: int | None = None
):
    # First check if the file is a replay file (extension .wowsreplay)
    if not file.filename.endswith(".wowsreplay"):
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=f"File is not a replay file ({file.filename})",
        )

    # Then check if the file is too large.
    if file.size > settings.replay.max_file_size:
        raise HTTPException(
            status_code=status.HTTP_413_REQUEST_ENTITY_TOO_LARGE,
            detail="File is too large"
        )

    # Log the new job.
    job_id = binascii.b2a_hex(os.urandom(7))
    logger.info(f"Started new render job: Job ID: {job_id}, Replay ID: {replay_id or 'None'}, "
                f"Filename: {file.filename}.")

    # Ensure the work folder exists, if not, create it.
    os.makedirs(settings.replay.temp_workdir, exist_ok=True)

    # Render time!
    replay_info = ReplayParser(file.file, True).get_info()
    replay_data: ReplayData = replay_info["hidden"]["replay_data"]

    # Concat filepaths to get the full path to the replay file.


    filepath = pathlib.Path(settings.replay.temp_workdir, f"{job_id}.mp4")
    renderer = Renderer(
        replay_data,
        enable_chat=True,
        team_tracers=True,
        target_player_id=target_player_id
    )
    renderer.start(str(filepath), quality=9, fps=30)

    background_tasks.add_task(os.remove, filepath)
    return FileResponse(filepath, media_type="video/mp4", filename=f"{replay_id or 'replay'}.mp4")