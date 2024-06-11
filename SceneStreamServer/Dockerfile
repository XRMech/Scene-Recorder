FROM node:18

# Create app directory
WORKDIR /usr/src/app

# Install global dependencies
RUN npm install -g node-pre-gyp

# Install app dependencies
COPY package*.json ./
RUN npm install

# Bundle app source
COPY . .

# Ensure uploads directory exists
RUN mkdir -p uploads

EXPOSE 3000
CMD [ "node", "server.js" ]
