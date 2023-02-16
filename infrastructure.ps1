docker-compose -f "docker-compose.yml" stop
docker-compose -f "docker-compose.yml" rm --force
docker-compose -f "docker-compose.yml" up --build localstack database.api kafka zookeeper elasticsearch