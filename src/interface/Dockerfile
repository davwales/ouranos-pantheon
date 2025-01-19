FROM node:18-alpine

WORKDIR /app

COPY ./src/interface/package.json .
RUN npm install

COPY ./src/interface .
RUN npm run build

EXPOSE 3000
CMD ["npm", "start"]