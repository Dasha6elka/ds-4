docker build -f api.dockerfile -t api:%1 .
docker build -f client.dockerfile -t client:%1 .
docker build -f logger.dockerfile -t logger:%1 .
docker build -f calc.dockerfile -t calc:%1 .

mkdir build_%1

cp scripts\start.cmd build_%1
cp scripts\stop.cmd build_%1
cp config\config.cmd build_%1