FROM python:3.13-bookworm AS development

RUN curl -sSL https://install.python-poetry.org | python3 -
ENV PATH="/root/.local/bin:$PATH"

FROM development AS build

WORKDIR /build

COPY Fluffle.Inference.Api/pyproject.toml Fluffle.Inference.Api/poetry.lock ./

ENV POETRY_VIRTUALENVS_IN_PROJECT="true"
RUN poetry install

FROM python:3.13-bookworm

WORKDIR /app

COPY --from=build /build/.venv .venv 
COPY Fluffle.Inference.Api/*.py ./

ENV VIRTUAL_ENV="/app/.venv"
ENV PATH="$VIRTUAL_ENV/bin:$PATH"

CMD [ "python", "-m", "fastapi", "run" ]
