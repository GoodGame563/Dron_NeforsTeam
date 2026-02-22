FROM postgres:18-alpine3.23


COPY init.sql /docker-entrypoint-initdb.d/01-init.sql


RUN chmod 644 /docker-entrypoint-initdb.d/01-init.sql


# COPY init-scripts/ /docker-entrypoint-initdb.d/