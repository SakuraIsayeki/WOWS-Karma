import binascii
import multiprocessing
import os
import pathlib

from fastapi import APIRouter, HTTPException, status, UploadFile, BackgroundTasks
from fastapi.logger import logger
from fastapi.responses import FileResponse
from renderer.data import ReplayData
from renderer.render import Renderer
from replay_parser import ReplayParser

from ..config import settings
from ..security import AuthenticatedUser

router = APIRouter()


def on_job_finished(job_id: str):
    filesize = os.stat(get_filepath(job_id, 'mp4')).st_size
    logger.info(f"Render job {job_id} finished. Filesize: {filesize / 1024 / 1024:.2f} MiB")
    os.remove(get_filepath(job_id, "wowsreplay"))
    os.remove(get_filepath(job_id, "mp4"))


def get_filepath(job_id: str, ext: str):
    return pathlib.Path(settings.replay.temp_workdir, f"{job_id}.{ext}")


def start_render(job_id: str, target_player_id: str | None):
    """
    Starts a minimap render job, optionally with a target player id and replay id
    :param job_id: The job id
    :param target_player_id: The target player id
    :return: None

    This method is designed to be run on another process.
    """
    with open(get_filepath(job_id, "wowsreplay"), "rb") as f:
        logger.info(f"File {get_filepath(job_id, 'wowsreplay')} opened. "
                    f"Filesize: {os.stat(get_filepath(job_id, 'wowsreplay')).st_size / 1024 / 1024:.2f} MiB")

        replay_info = ReplayParser(f, strict=True).get_info()
        replay_data: ReplayData = replay_info["hidden"]["replay_data"]

        renderer = Renderer(
            replay_data=replay_data,
            enable_chat=True,
            team_tracers=True,
            target_player_id=target_player_id,
        )

        renderer.start(
            path=str(get_filepath(job_id, "mp4")),
            quality=settings.replay.quality,
            fps=settings.replay.fps
        )


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
    job_id = str(binascii.b2a_hex(os.urandom(7))).split("'")[1]
    logger.info(f"Started new render job: Job ID: {job_id}, Replay ID: {replay_id or 'None'}, "
                f"Filename: {file.filename}.")

    file_content = await file.read()

    # Save the replay file to the work folder.
    with open(get_filepath(job_id, "wowsreplay"), "xb") as f:
        f.write(file_content)

        logger.info(f"Saved replay file to {get_filepath(job_id, 'wowsreplay')}")
        logger.info(f"Filesize: {os.stat(get_filepath(job_id, 'wowsreplay')).st_size / 1024 / 1024:.2f} MiB")

    # Start the render job on another process.

    context = multiprocessing.get_context('fork')
    process = context.Process(target=start_render, args=(job_id, target_player_id))
    process.start()
    process.join()  # Wait for the process to finish.

    filepath = get_filepath(job_id, "mp4")
    background_tasks.add_task(on_job_finished, job_id)

    # Log the finished job.
    logger.info(f"Render job {job_id} finished. Filesize: {os.stat(filepath).st_size / 1024 / 1024:.2f} MiB")

    return FileResponse(
        path=filepath,
        media_type="video/mp4",
        filename=f"{replay_id or 'replay'}.mp4"
    )