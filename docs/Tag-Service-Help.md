---
title: Tag Service Help
categories: [guide]
layout: topic
---

The tag service provides commands that allow users to post messages that may be needed frequently. For example, linking to documentation could be done with a tag, instead of putting it in pins or somewhere else.

All commands in this group have no group prefix, just the command prefix.

Tags *usually* must have a unique name. In the same channel tags must have a unique name, but across channels they can share names. Tags created in that channel with identical names take priority over ones created in channels. Tags do not transfer across servers.

## Tag

Replies with the contents of a tag, if it exists.
The reply will be in the format:
`<tag name> (<tag ID>): <tag value>`

Parameters:
 - Name: The name of the tag to show, if it exists. Tags must not contain any whitespace, so this should just be a single word.

 Example: `?tag tias`

## Make Tag

**Requires the user permission to Manage Messages.**

Makes a new tag, if the tag name hasn't been used before.
Users who are banned from making tags cannot make tags.

The OK emoji means it worked, the hammer emoji means you are banned.

Parameters:
 - Name: The name of the tag to show, which is typically a single word that doesn't contain whitespace.
 - Contents: The contents of the tag. This can be indefinitely long, doesn't have to be surrounded with quotes, and can contain anything.

Example: `?tag ping pong`

## Delete Tag

**Requires the user permission to Manage Messages.**

Deletes a tag _by the tag ID number_. This is often included in the output of `?tag`.

Aliases:
 - `deletetag`
 - `deltag`

Parameters:
 - Tag ID: The ID number of the tag to ban as an integer value.

Example: `?deletetag 123`

## Ban Tag

**Requires the user permission to Manage Messages.**

Bans a user from using the tag system. Users that
have the Manage Messages permission or above can not be
banned. (If that's the case, you should demote them first.)

Banned users will have _all_ of their authored tags
in the current server removed.

Tag bans are only on a server-by-server basis.

Aliases:
 - `bantag`
 - `tagban`

Parameters:
 - User: This can either be an `@Mention` of a user, or their Discord User ID.

Example: `?bantag @ChrisJ#8703`, `?bantag 123123123`

## Unban Tags

**Requires user Administrator permissions.**

Un-bans a user from using the tag system on the current
server.

Aliases:
 - `unbantag`
 - `tagunban`

Parameters:
 - User: This can either be an `@Mention` of a user, or their Discord User ID.

Example: `?unbantag @ChrisJ#8703`, `?unbantag 123123123`

## Tag information

Gets the information about a tag, including its ID,
author and creation time, if it is found.

Aliases:
 - `taginfo`

Parameters:
 - Name: The name of the tag to show, which is typically a single word that doesn't contain whitespace.

Example: `?taginfo ping`

## Tag List

Lists all of the tags in the server.

Aliases:
 - `taglist`
 - `listtags`
 - `listtag`

Example: `?taglist`

## User Tags

List all of the tags created by a user in this server.

Aliases:
 - `usertags`
 - `taguser`

Parameters:
 - User [Optional]: The user to list tags for. Either specify with `@Mention` or by User ID. If omitted, uses the user that authored the command (yourself?).

Example: `?usertags`, `?usertags @ChrisJ#8703`
