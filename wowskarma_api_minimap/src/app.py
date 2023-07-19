import io
import os

from fastapi import FastAPI
from starlette.middleware.cors import CORSMiddleware

from .config import settings
from .db import create_db_and_tables, engine
from .routes import main_router


def read(*paths, **kwargs):
    """Read the contents of a text file safely.
    >>> read("VERSION")
    """
    content = ""
    with io.open(
        os.path.join(os.path.dirname(__file__), *paths),
        encoding=kwargs.get("encoding", "utf8"),
    ) as open_file:
        content = open_file.read().strip()
    return content


description = """
Standalone Minimap rendering microservice to render World of Warships replays.
"""

app = FastAPI(
    title="WOWS Karma - Minimap API",
    description=description,
    version=read("VERSION"),
    contact={
        "name": "Sakura Isayeki",
        "url": "https://github.com/SakuraIsayeki",
        "email": "sakura.isayeki@nodsoft.net",
    },
)

if settings.server and settings.server.get("cors_origins", None):
    app.add_middleware(
        CORSMiddleware,
        allow_origins=settings.server.cors_origins,
        allow_credentials=settings.get("server.cors_allow_credentials", True),
        allow_methods=settings.get("server.cors_allow_methods", ["*"]),
        allow_headers=settings.get("server.cors_allow_headers", ["*"]),
    )

app.include_router(main_router)


@app.on_event("startup")
def on_startup():
    create_db_and_tables(engine)
