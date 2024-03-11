***** Install *****
The installation files are in the "install" folder in github ( https://github.com/DoniDaniela/PAD3/tree/main/Service/install ) 
Download docker-compose.override.yml and docker-compose.yml files from github from the install folder, download and save in a local folder on Local Disk (C:) , open command prompt. (make sure you have docker desktop)
In CMD, write cd and the path where the folder with the files is, and give the commands: docker-compose build and docker-compose up. (it might take some minutes) after the command has been run, check in docker desktop if all the containers are launched.

***** Postman Collection *****
To test the service, download the postman collection and launch it in the Postman application (https://github.com/DoniDaniela/PAD3/blob/main/PAD3.postman_collection.json)

***** Redis *****
Pentru a vedea datele din redis avem nevoie de un Redis GUI
https://github.com/qishibo/AnotherRedisDesktopManager/releases
in GUI configuram accesul local la Redis pentru adresa: localhost@6379

***** Docker hub *****
https://hub.docker.com/repository/docker/danieladoni01/service.api

***** Test with Swagger*****
Link-ul de lansare a serviciului dupa instalare
http://localhost:7101/swagger/index.html

