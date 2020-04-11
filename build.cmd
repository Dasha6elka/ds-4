docker build -f api.dockerfile -t api:%1 .
docker build -f client.dockerfile -t client:%1 .
docker build -f logger.dockerfile -t logger:%1 .

mkdir application%1

cp scripts\start.cmd application%1
cp scripts\stop.cmd application%1
cp config\config.cmd application%1