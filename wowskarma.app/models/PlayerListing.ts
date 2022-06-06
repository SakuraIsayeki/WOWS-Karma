/**
 * Defines a player listing, comprised of a player's ID and username.
 * @class
 * @property {number} id - The player's ID (uint32, unique).
 * @property {string} username - The player's username (alphanumeric string, unique).
 */
export class PlayerListing {
    readonly id: number;
    readonly username: string;

    constructor(id: number, username: string) {
        this.id = id;
        this.username = username;
    }
}