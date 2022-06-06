import {useState} from "react";
import useSWR, { useSWRConfig } from "swr";
import {container, inject} from "tsyringe";
import {PlayerClient} from "../../modules/api/PlayerClient";


export default function PlayerSearchResults({username}: { username: string }) {
    const playerClient = container.resolve(PlayerClient);
    const { cache, mutate, ...extraConfig } = useSWRConfig()

    const path = `/api/player/search/${username}`;
    const { data, error } = useSWR(path, username => playerClient.searchPlayers(username));

    if (error) { return <h4 className="text-danger my-5">Error: {error.message}</h4>; }
    if (!data) { return <h4 className="text-info my-5">Searching...</h4>; }
    if (data.length === 0) { return <h4 className="text-warning">No players found. Try again.</h4>; }

    return (
        <>
            <h4 className="text-success mb-3">Found {data.length} Account(s) :</h4>

            <ul>
                {data.map(player => (
                    <li key={player.id}>
                        <a href={`/player/${player.id},${player.username}`}>{player.username}</a>
                    </li>
                ))}
            </ul>
        </>
    )
}