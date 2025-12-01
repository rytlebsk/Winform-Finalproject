# Run Backend

To run the backend server, navigate to the `backend` directory and execute:

```bash
cd backend
cargo run
```

If you need a clean database, you can run:

```bash
cargo run -- clear
```

# Active Database

First, install PostgreSQL if you haven't already.

Make sure to have the database service running. You use this command to initialize PostgreSQL database:

```bash
initdb -D db_data -U postgres -A trust --locale=C --encoding=UTF8
```

Then, start the PostgreSQL server with:

```bash
postgres -D db_data -p 5433
```

The database will be accessible at `localhost:5433`.
