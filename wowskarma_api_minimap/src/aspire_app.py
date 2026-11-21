import os

from sqlmodel import Session, select

from .app import app
from .db import engine
from .security import User, get_password_hash, verify_password


@app.on_event("startup")
def provision_api_user():
    username = os.environ["WOWSKARMA_MINIMAP_LOGIN"]
    password = os.environ["WOWSKARMA_MINIMAP_PASSWORD"]

    with Session(engine) as session:
        user = session.exec(select(User).where(User.username == username)).one_or_none()
        if user is None:
            session.add(User(username=username, password=password))
        elif not verify_password(password, user.password):
            user.password = get_password_hash(password)
        session.commit()
