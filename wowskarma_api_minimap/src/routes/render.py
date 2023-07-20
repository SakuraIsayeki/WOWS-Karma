import base64
import binascii
import random
import tempfile
import os

from fastapi import APIRouter, HTTPException, status, UploadFile
from fastapi.responses import FileResponse
from renderer.data import ReplayData
from renderer.render import Renderer
from replay_parser import ReplayParser

from ..config import settings
from ..security import AuthenticatedUser

router = APIRouter()


@router.post("/render", dependencies=[AuthenticatedUser])
async def render_replay(file: UploadFile, replay_id: str | None = None):
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
            detail="File is too large",
        )

    # We're gonna write the replay data to a temporary file, in the following folder:
    # /tmp/wows-karma/minimap/

    # Ensure the folder exists, if not, create it.
    os.makedirs("/tmp/wows-karma/minimap/", exist_ok=True)

    # Render time!
    replay_info = ReplayParser(file.file, True).get_info()
    replay_data: ReplayData = replay_info["hidden"]["replay_data"]

    filepath = f"/tmp/wows-karma/minimap/{binascii.b2a_hex(os.urandom(7))}.mp4"
    Renderer(replay_data, enable_chat=True, team_tracers=True).start(filepath)

    return FileResponse(filepath, media_type="video/mp4", filename=f"{replay_id or 'replay'}.mp4")