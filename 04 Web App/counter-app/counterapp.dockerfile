# syntax=docker/dockerfile:1

FROM node:16
WORKDIR /usr/app
RUN npm install -g @angular/cli@7.1.3
RUN npm install @aspnet/signalr
RUN ng new counter-app
COPY ./src /counter-app/
WORKDIR /usr/app/counter-app
RUN ng serve & sleep 20
