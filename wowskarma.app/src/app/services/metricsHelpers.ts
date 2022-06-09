/**
 *
 */
import { PostFlairs } from "./api/models/post-flairs";

/**
 * Returns a nullable boolean indicating whether the post has specified flair,
 * and if so, whether it is positive or negative.
 * @param flairs The post flairs
 * @param positive The positive flair to check for
 * @param negative The negative flair to check for
 * @returns A nullable boolean indicating the flair's presence and value:
 *
 *  - `true` if the post has the positive flair.
 *  - `false` if the post has the negative flair.
 *  - `null` if the post does not have the specified flair.
 *
 * @see https://github.com/SakuraIsayeki/WOWS-Karma/blob/main/WowsKarma.Common/Models/PostFlairs.cs
 */
function parseBalancedFlags(flairs: PostFlairs, positive: PostFlairs, negative: PostFlairs): boolean | null {
  if ((flairs & positive) !== 0) {
    return true;
  } else if ((flairs & negative) !== 0) {
    return false;
  } else {
    return null;
  }
}

/**
 * Removes conflicting flairs from a post.
 * @param flairs The post flairs
 * @param positive The positive flair to check for
 * @param negative The negative flair to check for
 * @returns The post flairs with the conflicting flairs removed.
 * @see https://github.com/SakuraIsayeki/WOWS-Karma/blob/main/WowsKarma.Common/Models/PostFlairs.cs
 */
function removeConflictingFlairs(flairs: PostFlairs, positive: PostFlairs, negative: PostFlairs): PostFlairs {
  return flairs & ~(positive | negative);
}

/**
 * Counts the number of positive and negative flairs in a post.
 * @param flairs The post flairs, arranged as an array of nullable booleans.
 * @returns A signed integer indicating the number of positive and negative flairs.
 * @see https://github.com/SakuraIsayeki/WOWS-Karma/blob/main/WowsKarma.Common/Models/PostFlairs.cs
 */
export function countBalance([performance, teamplay, courtesy]: [boolean | null, boolean | null, boolean | null]): number {
  let balance = 0;

  for (const value of [performance, teamplay, courtesy]) {
    if (value === true) {
      balance++;
    } else if (value === false) {
      balance--;
    }
  }

  return balance;
}

/**
 * Parses the post flairs into a balanced array of nullable booleans.
 * @param flairs The post flairs, in integer (enum flags) form.
 * @returns An array of nullable booleans indicating the presence of positive and negative flairs.
 * @see https://github.com/SakuraIsayeki/WOWS-Karma/blob/main/WowsKarma.Common/Models/PostFlairs.cs
 */
export function parseFlairsEnum(flairs: PostFlairs | null): [boolean | null, boolean | null, boolean | null] {
  if (flairs == null) {
    return [null, null, null];
  }

  const performance = parseBalancedFlags(flairs, PostFlairs.PerformanceGood, PostFlairs.PerformanceBad);
  const teamplay = parseBalancedFlags(flairs, PostFlairs.TeamplayGood, PostFlairs.TeamPlayBad);
  const courtesy = parseBalancedFlags(flairs, PostFlairs.CourtesyGood, PostFlairs.CourtesyBad);

  return [performance, teamplay, courtesy];
}

/**
 * Parses the post flairs from a nullable boolean array, into an enum flags form.
 * @param flairs The post flairs, in nullable boolean array form.
 * @returns The post flairs, in integer (enum flags) form.
 * @see https://github.com/SakuraIsayeki/WOWS-Karma/blob/main/WowsKarma.Common/Models/PostFlairs.cs
 */
export function toEnum(flairs: [boolean | null, boolean | null, boolean | null]): PostFlairs {
  let flairsEnum = 0;

  if (flairs[0] === true) {
    flairsEnum |= PostFlairs.PerformanceGood;
  } else if (flairs[0] === false) {
    flairsEnum |= PostFlairs.PerformanceBad;
  }

  if (flairs[1] === true) {
    flairsEnum |= PostFlairs.TeamplayGood;
  } else if (flairs[1] === false) {
    flairsEnum |= PostFlairs.TeamPlayBad;
  }

  if (flairs[2] === true) {
    flairsEnum |= PostFlairs.CourtesyGood;
  } else if (flairs[2] === false) {
    flairsEnum |= PostFlairs.CourtesyBad;
  }

  return flairsEnum;
}

/**
 * Removes any conflicting flairs from a post.
 * @param flairs The post flairs, in enum flags form.
 * @returns The post flairs, with conflicting flairs removed.
 * @see https://github.com/SakuraIsayeki/WOWS-Karma/blob/main/WowsKarma.Common/Models/PostFlairs.cs
 */
export function sanitizeFlags(flairs: PostFlairs) {
  flairs = removeConflictingFlairs(flairs, PostFlairs.PerformanceGood, PostFlairs.PerformanceBad);
  flairs = removeConflictingFlairs(flairs, PostFlairs.TeamplayGood, PostFlairs.TeamPlayBad);
  flairs = removeConflictingFlairs(flairs, PostFlairs.CourtesyGood, PostFlairs.CourtesyBad);
  return flairs;
}
