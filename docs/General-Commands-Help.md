---
title: General Commands Help
categories: [guide]
layout: topic
---

This page will list information to use the general commands.

All examples are case-insensitive. `?ping` will work the same as `?PING`.

## Help

Replies back with a listing of all of the commands, and what parameters they take. Tries to direct message the user that sent the command.

The help text isn't very good, this wiki may prove to be
more useful.

Example: `?help`

## Help Here

Effectively the same as Help, but will post the help text in the same channel that the command was received in.

Example: `?helphere`

## Ping

A basic ping/pong command that replies back with the text "Pong!".

Example: `?ping`

## Echo

A simple echo command that replies back with the text that you provide.

Parameters:
 - Text: Whatever you want the bot to echo back to you.
 There is no need to enclose this text in double quotes.

Example: `?echo test`

## About / GitHub

Replies back with a link to this GitHub repository,
where users can find more information about the bot.

Aliases:
 - `About`
 - `GitHub`

Example: `?about`, `?github`

## Cleanup

**Requires the Manage Messages permission.**

Searches through an amount of messages less than 25,
if they were sent by CSSBot, then they get deleted.
Also will attempt to delete the message that issued the
command.

Parameters:
 - Amount: An integer number [0-25] of the number
 of messages to go through.

Example: `?cleanup 25`

## Invite Link

**Requires the user permission to embed links.**

Replies back with text explaining how to invite the bot to the server, and the invite link for it.

Aliases:
 - `InviteLink`

Example: `?InviteLink`

## Debug

Replies back with debug information about the bot,
including it's uptime, memory usage, and count of number
of servers it is in.

Example: `?debug`
