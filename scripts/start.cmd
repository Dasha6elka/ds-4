call config.cmd
docker run --rm -d -p %REDIS_PORT%:6379 --name redis redis:5-alpine
docker run --rm -d -p %NATS_PORT%:4222 --name nats nats:2-alpine
docker run --rm -d --env REDIS_HOST=%REDIS_HOST%:%REDIS_PORT% --env NATS_HOST=%NATS_HOST%:%NATS_PORT% --link %REDIS_HOST% --link %NATS_HOST% --name logger logger:%1
docker run --rm -d --env REDIS_HOST=%REDIS_HOST%:%REDIS_PORT% --env NATS_HOST=%NATS_HOST%:%NATS_PORT% --link %REDIS_HOST% --link %NATS_HOST% -p %API_PORT%:5000 --name api api:%1
docker run --rm -d --env API_HOST=%API_HOST%:%API_PORT% --link %API_HOST% -p %CLIENT_PORT%:80 --name client client:%1