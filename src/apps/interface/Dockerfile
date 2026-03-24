FROM node:22-alpine

WORKDIR /app

COPY ./src/apps/interface/package*.json ./
RUN npm install

COPY ./src/apps/interface .
RUN npm run build

EXPOSE 3000
CMD ["npm", "start"]