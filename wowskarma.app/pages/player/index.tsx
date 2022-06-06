import type { NextPage } from "next";
import {FormEvent, useState} from "react";
import {container} from "tsyringe";
import {PlayerClient} from "../../modules/api/PlayerClient";
import {PlayerListing} from "../../models/PlayerListing";
import PlayerSearchResults from "../../components/player/PlayerSearchResults";


export const Index: NextPage = () => {
    const playerClient = container.resolve(PlayerClient);
    const [submitted, setSubmitted] = useState(false);
    const [username, setUsername] = useState("");


    const HandleSubmit = async (e: FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setUsername(e.currentTarget.username.value);
        setSubmitted(true);
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

            {
                submitted && <div className="mx-3 my-5">
                    <PlayerSearchResults username={username} />
                </div>
            }
        </>
    )
}

export function DisplayPlayers(submitted: boolean, data: PlayerListing[], error: any) {
    if (!submitted) { return null; }

}

export default Index;