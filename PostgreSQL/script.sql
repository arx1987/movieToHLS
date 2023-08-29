create table if not exists users
(
    id      uuid         not null
        constraint users_pk
            primary key,
    chat_id varchar(255) not null
);

alter table users
    owner to postgres;

create table if not exists torrents
(
    id    uuid         not null
        constraint torrents_pk
            primary key,
    slug  varchar(255) not null,
    title text
);

alter table torrents
    owner to postgres;

create table if not exists torrent_access
(
    id         uuid not null
        primary key,
    torrent_id uuid
        constraint torrent_access_torrents_id_fk
            references torrents
            on delete cascade,
    user_id    uuid
        constraint torrent_access_users_id_fk
            references users
            on delete cascade
);

alter table torrent_access
    owner to postgres;