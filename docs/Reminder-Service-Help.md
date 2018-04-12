---
title: Reminder Service Help
categories: [guide]
layout: topic
---

The reminder service provides a way to post a reminder for a specific date and time
about an event, homework, or whatever else. Reminders will update the channel
on some set intervals before they expire, to remind them in advance before they expire.

By default, reminders will update 1 Day before, 1 Hour before and when the reminder expires.
Any interval of time (including notifications _after_ the expiration time) should work,
so reminders could push a notification 3 hours overdue, for example.

All of these commands have a group prefix of "Reminder", with an alias of "R".
This means that you can type `?Reminder ...` and `?R ...` and they should
behave the same.

## Add / Create / + / AddReminder / CreateReminder / NewReminder

Makes a new reminder with the given date/time and description.

Aliases:
 - `add`
 - `create`
 - `+`
 - `addreminder`
 - `createreminder`
 - `newreminder`

Parameters:
 - Date and Time: This _must_ be surrounded in double quotes. If the date is not specified,
 it will default to the current date. If the time is specified (I think) the default is
 midnight. Examples of this are: `"5PM"`, `"1/2/2017 11PM"`, `"4/11/18 9:01PM"`
 - Text: The contents of the reminder text. This does not have to be surrounded by double quotes.
 This can also contain links. Formatting and server emoji may not be guaranteed to work
 correctly.

Example: `?r add "11PM" Make better docs for CSSBot.`

## Add Channel / Create Channel / +Channel

**Requires the Manage Messages user permission.**

Makes a new _channel_ reminder with the given date/time and description.
_Channel_ reminders will mention `@here` when they notify and expire.

Aliases:
 - `AddChannel`
 - `CreateChannel`
- `+Channel`

Parameters:
- Date and Time: This _must_ be surrounded in double quotes. If the date is not specified,
it will default to the current date. If the time is specified (I think) the default is
midnight. Examples of this are: `"5PM"`, `"1/2/2017 11PM"`, `"4/11/18 9:01PM"`
- Text: The contents of the reminder text. This does not have to be surrounded by double quotes.
This can also contain links. Formatting and server emoji may not be guaranteed to work
correctly.

Example: `?r addchannel "11PM" Make better docs for CSSBot.`

## Add Server / Create Server / +Server / AddGuild / CreateGuild / +Guild

**Requires the Manage Messages user permission.**

Makes a new _guild_ reminder with the given date/time and description.
_guild_ reminders will mention `@everyone` when they notify and expire.

Aliases:
 - `AddServer`
 - `CreateServer`
 - `+Server`
 - `AddGuild`
 - `CreateGuild`
 - `+Guild`

Parameters:
- Date and Time: This _must_ be surrounded in double quotes. If the date is not specified,
it will default to the current date. If the time is specified (I think) the default is
midnight. Examples of this are: `"5PM"`, `"1/2/2017 11PM"`, `"4/11/18 9:01PM"`
- Text: The contents of the reminder text. This does not have to be surrounded by double quotes.
This can also contain links. Formatting and server emoji may not be guaranteed to work
correctly.

Example: `?r addguild "11PM" Make better docs for CSSBot.`

## List Type Options / List Types / List Type

Lists all the valid types of notifications.
Normally, these are `Author` (`@mentions` the author of the reminder when it updates),
`Channel` (mentions `@here`), `Guild` (mentions `@everyone`), and `Default` (no mentions).

Aliases:
 - `ListTypeOptions`
 - `ListTypes`
 - `ListType`

Example: `?r listtype`

## Add Reminder Timespan

`todo`
