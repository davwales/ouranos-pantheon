FROM node:26.12-alpine

WORKDIR /app

COPY ./src/apps/interface/package*.json ./
RUN npm ci

COPY ./src/apps/interface .
RUN npm run build

EXPOSE 3000
CMD ["npm", "start"]