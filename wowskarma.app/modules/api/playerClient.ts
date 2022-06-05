import {ApiClientBase} from "./apiClientBase";
import {PlayerListing} from "../../models/playerListing";
import {autoInjectable, inject} from "tsyringe";

/**
 * Defines an API client for player-related endpoints.
 * Inherits from the {@link ApiClientBase} class.
 * @extends ApiClientBase
 * @class
 */

@autoInjectable()
export class PlayerClient extends ApiClientBase {
    private readonly _searchEndpoint = new URL("player/search/", this.Host);

    /**
     * Fetches a list of all players starting with the specified name.
     * List is limited to the first 100 results.
     * @param {string} name - The name to search for. Must be at least 3 characters.
     * @returns {Promise<PlayerListing[]>}
     * @memberof PlayerClient
     * @method
     */
    public async searchPlayers(name: string): Promise<PlayerListing[]> {
        if (name.length < 3) {
            throw new Error("Name must be at least 3 characters.");
        }

        const response = await fetch(new URL(name, this._searchEndpoint));

        if (!response.ok) {
            throw new Error(`Failed to fetch player list. Status: ${response.status}`);
        }

        const json = await response.json();
        return json.map((player: any) => new PlayerListing(player.id, player.username));
    }
}