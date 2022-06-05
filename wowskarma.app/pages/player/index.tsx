import type { NextPage } from "next";
import React from "react";
import {container} from "tsyringe";
import {PlayerClient} from "../../modules/api/playerClient";
import useSWR from "swr";
import {PlayerListing} from "../../models/playerListing";

export const Index: NextPage = () => {
    let submitted = false, data: PlayerListing[] = [], error;

    const HandleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const playerClient: PlayerClient = container.resolve<PlayerClient>(PlayerClient);
        const {username} = { username: e.currentTarget.username.value };

        submitted = true;
        const {data: _data, error: _error } = useSWR(`/api/player/search/${username}`, username => playerClient.searchPlayers(username));

        data = _data || [];
        error = _error;
    }



    return (
        <>
            <div className="col-md-6">
                <h1 className="mb-3">Search Players</h1>

                <form onSubmit={HandleSubmit}>
                    <div className="input-group">
                        <input id="username" type="text" className="form-control" placeholder="Username" required minLength={3} />
                        <button type="submit" className="btn btn-primary px-3">Search</button>
                    </div>
                </form>
            </div>

            <div className="mx-3 my-5">
                {DisplayPlayers(submitted, data, error)}
            </div>
        </>
    )
}

function DisplayPlayers(submitted: boolean, data: PlayerListing[], error: any) {
    if (!submitted) { return null; }
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

export default Index;