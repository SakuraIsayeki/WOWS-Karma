# WOWS Karma - Minimap API

[![codecov](https://codecov.io/gh/SakuraIsayeki/wowskarma.api.minimap/branch/main/graph/badge.svg?token=wowskarma.api.minimap_token_here)](https://codecov.io/gh/SakuraIsayeki/wowskarma.api.minimap)
[![CI](https://github.com/SakuraIsayeki/wowskarma.api.minimap/actions/workflows/main.yml/badge.svg)](https://github.com/SakuraIsayeki/wowskarma.api.minimap/actions/workflows/main.yml)

Standalone Minimap rendering microservice to render World of Warships replays.

## Install

from source
```bash
git clone https://github.com/SakuraIsayeki/WOWS-Karma wows-karma
cd wows-karma/wowskarma.api.minimap
make install
```

from pypi

```bash
pip install wowskarma.api.minimap
```

## Executing

```bash
$ wowskarma.api.minimap run --port 8080
```

or

```bash
python -m wowskarma.api.minimap run --port 8080
```

or

```bash
$ uvicorn wowskarma.api.minimap:app
```

## CLI

```bash
❯ wowskarma.api.minimap --help
Usage: wowskarma.api.minimap [OPTIONS] COMMAND [ARGS]...

Options:
  --install-completion [bash|zsh|fish|powershell|pwsh]
                                  Install completion for the specified shell.
  --show-completion [bash|zsh|fish|powershell|pwsh]
                                  Show completion for the specified shell, to
                                  copy it or customize the installation.
  --help                          Show this message and exit.

Commands:
  create-user  Create user
  run          Run the API server.
  shell        Opens an interactive shell with objects auto imported
```

### Creating a user

```bash
❯ wowskarma.api.minimap create-user --help
Usage: src create-user [OPTIONS] USERNAME PASSWORD

  Create user

Arguments:
  USERNAME  [required]
  PASSWORD  [required]

Options:
  --superuser / --no-superuser  [default: no-superuser]
  --help 
```

**IMPORTANT** To create an admin user on the first run:

```bash
wowskarma.api.minimap create-user admin admin --superuser
```

### The Shell

You can enter an interactive shell with all the objects imported.

```bash
❯ wowskarma.api.minimap shell       
Auto imports: ['app', 'settings', 'User', 'engine', 'cli', 'create_user', 'select', 'session', 'Content']

In [1]: session.query(Content).all()
Out[1]: [Content(text='string', title='string', created_time='2021-09-14T19:25:00.050441', user_id=1, slug='string', id=1, published=False, tags='string')]

In [2]: user = session.get(User, 1)

In [3]: user.contents
Out[3]: [Content(text='string', title='string', created_time='2021-09-14T19:25:00.050441', user_id=1, slug='string', id=1, published=False, tags='string')]
```

## API

Run with `wowskarma.api.minimap run` and access http://127.0.0.1:8000/docs

![](https://raw.githubusercontent.com/rochacbruno/fastapi-project-template/master/docs/api.png)


**For some api calls you must authenticate** using the user created with `wowskarma.api.minimap create-user`.

## Testing

``` bash
❯ make test
Black All done! ✨ 🍰 ✨
13 files would be left unchanged.
Isort All done! ✨ 🍰 ✨
6 files would be left unchanged.
Success: no issues found in 13 source files
================================ test session starts ===========================
platform linux -- Python 3.9.6, pytest-6.2.5, py-1.10.0, pluggy-1.0.0 -- 
/fastapi-project-template/.venv/bin/python3
cachedir: .pytest_cache
rootdir: /fastapi-project-template
plugins: cov-2.12.1
collected 10 items                                                                                                                               

tests/test_app.py::test_using_testing_db PASSED                           [ 10%]
tests/test_app.py::test_index PASSED                                      [ 20%]
tests/test_cli.py::test_help PASSED                                       [ 30%]
tests/test_cli.py::test_cmds_help[run-args0---port] PASSED                [ 40%]
tests/test_cli.py::test_cmds_help[create-user-args1-create-user] PASSED   [ 50%]
tests/test_cli.py::test_cmds[create-user-args0-created admin2 user] PASSED[ 60%]
tests/test_content_api.py::test_content_create PASSED                     [ 70%]
tests/test_content_api.py::test_content_list PASSED                       [ 80%]
tests/test_user_api.py::test_user_list PASSED                             [ 90%]
tests/test_user_api.py::test_user_create PASSED                           [100%]

----------- coverage: platform linux, python 3.9.6-final-0 -----------
Name                              Stmts   Miss  Cover
-----------------------------------------------------
wowskarma.api.minimap/__init__.py              4      0   100%
wowskarma.api.minimap/app.py                  16      1    94%
wowskarma.api.minimap/cli.py                  21      0   100%
wowskarma.api.minimap/config.py                5      0   100%
wowskarma.api.minimap/db.py                   10      0   100%
wowskarma.api.minimap/models/__init__.py       0      0   100%
wowskarma.api.minimap/models/content.py       47      1    98%
wowskarma.api.minimap/routes/__init__.py      11      0   100%
wowskarma.api.minimap/routes/content.py       52     25    52%
wowskarma.api.minimap/routes/security.py      15      1    93%
wowskarma.api.minimap/routes/user.py          52     26    50%
wowskarma.api.minimap/security.py            103     12    88%
-----------------------------------------------------
TOTAL                               336     66    80%


========================== 10 passed in 2.34s ==================================

```

## Linting and Formatting

```bash
make lint  # checks for linting errors
make fmt   # formats the code
```


## Configuration

This project uses [Dynaconf](https://dynaconf.com) to manage configuration.

```py
from wowskarma.api.minimap.config import settings
```

## Acessing variables

```py
settings.get("SECRET_KEY", default="sdnfjbnfsdf")
settings["SECRET_KEY"]
settings.SECRET_KEY
settings.db.uri
settings["db"]["uri"]
settings["db.uri"]
settings.DB__uri
```

## Defining variables

### On files

settings.toml

```toml
[development]
dynaconf_merge = true

[development.db]
echo = true
```

> `dynaconf_merge` is a boolean that tells if the settings should be merged with the default settings defined in wowskarma.api.minimap/default.toml.

### As environment variables
```bash
export wowskarma.api.minimap_KEY=value
export wowskarma.api.minimap_KEY="@int 42"
export wowskarma.api.minimap_KEY="@jinja {{ this.db.uri }}"
export wowskarma.api.minimap_DB__uri="@jinja {{ this.db.uri | replace('db', 'data') }}"
```

### Secrets

There is a file `.secrets.toml` where your sensitive variables are stored,
that file must be ignored by git. (add that to .gitignore)

Or store your secrets in environment variables or a vault service, Dynaconf
can read those variables.

### Switching environments

```bash
wowskarma.api.minimap_ENV=production wowskarma.api.minimap run
```

Read more on https://dynaconf.com

## Development

Read the [CONTRIBUTING.md](CONTRIBUTING.md) file.
