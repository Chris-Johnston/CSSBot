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

**Requires the Manage Messages permission, or that you are the author of a
reminder being edited.**

Adds a timespan before the expiration of a Reminder to the list of times
for the Reminder to send a notification.

For example, if your reminder expires at midnight, and you wish to be notified
2 hours beforehand, you would add a timespan of "2:00:00", or 2 hours.
In addition, you can also add notifications after the reminder expires.
To be notified 2 hours after the expiration, you would add a timespan of "-2:00:00",
or -2 hours.

If the timespan added would already be expired when it is added, it will
not be added.

Parameters:
- Reminder ID: The ID number of the reminder you wish to modify
- TimeSpan: The amount of time before the reminder's expiration that you wish
to be notified beforehand. Use a negative value to be notified after the
reminder expires.

Aliases:
- `AddReminderTimespan`
- `AddTimespan`
- `AddTime`
- `AddUpdateTime`
- `AddUpdate`

Example: `?r addremindertimespan 123 "1.2:00:00"` `?r addremindertimespan 123 "-3:00:00"`

## Remove Reminder Timespan

**Requires the Manage Messages permission, or that you are the author of a reminder
being edited.**

Removes a matching timespan from a reminder.

Parameters:
- Reminder ID: The ID number of the reminder you wish to modify
- TimeSpan: The amount of time before the reminder's expiration that you wish
to be notified beforehand. Use a negative value to be notified after the
reminder expires.

Aliases:
- `RemoveReminderTimespan`
- `RemoveTimespan`
- `RemoveTime`
- `RemoveUpdateTime`
- `RemoveUpdate`

Example: `?r removeupdate 123 "1.2:00:00"` `?r removetime 123 "-3:00:00"`

## Update Text

**Requires the Manage Messages permission, or that you are the author of a reminder
being edited.**

Sets the text for a reminder.

Parameters:
- Reminder ID: The ID number of the reminder you wish to modify.
- Text: Does not have to be enclosed in double quotes (`""`). The content
of the reminder's body.

Aliases:
- `UpdateText`
- `ChangeText`

Example: `?r UpdateText 123 this is some new reminder text`

## Update Time

**Requires the Manage Messages permission, or that you are the author of a reminder
being edited.**

Sets the time for a reminder.

Parameters:
- Reminder ID: The ID number of the reminder you wish to modify.
- Time: The DateTime when the reminder should expire.

Aliases:
- `UpdateTime`
- `ChangeTime`

Example: `?r UpdateTime 123 "4/16/18 5PM"`

## Update Type

**Requires the Manage Messages permission, or that you are the author of a reminder
being edited.**

Changes the type of reminder.

Parameters:
- Reminder ID: The ID number of the reminder you wish to modify.
- ReminderType: The type of reminder, as a single word that does not have to be
enclosed in double quotes (`""`).

Aliases:
- `UpdateType`
- `ChangeType`

Example: `?r ChangeType 123 Author`

## Dismiss Reminder

**Requires the Manage Messages permission, or that you are the author of a reminder
being edited.**

Closes a reminder. This action cannot be undone. All of the reminder data
is permanently deleted.

Parameters:
- Reminder ID: The ID number of the reminder you wish to modify.

Aliases:
- `Dismiss`
- `DismissReminder`
- `End`
- `Remove`
- `Delete`

Example: `?r Dismiss 123`

## Get Reminder Info

Gets the info about a reminder by it's ID.

Parameters:
- Reminder ID: The ID number of the reminder you wish to get info for.

Aliases:
- `GetReminder`
- `Get`

Example: `?r get 123`

## Get All Reminders in Server

Gets all of the reminders in a server. *This will also include
any reminders that have been created in private channels, and can expose
the names of private channels.*

Aliases:
- `GuildReminders`
- `ServerReminders`
- `ListServer`
- `ListGuild`

Example: `?r listguild`

## Get All Reminders in Channel

Gets all of the reminders of a text channel.

Parameters:
- Channel: A `#channel` mention, or the channel ID. If no value is supplied,
assumes to use the current channel where the command was sent.

Aliases:
- `ChannelReminders`
- `Reminders`
- `ListChannel`

Example: `?r reminders`

## Get All Reminders for User

Gets all of the reminders for a user in a guild. *This will also include
any reminders that have been created in private channels, and can expose
the names of private channels.*

Parameters:
- User: A `@Mention` of a user, or their user ID. If no value is supplied,
assumes to use the user which authored the command message.

Aliases:
- `UserReminders`
- `MyReminders`
- `ListUser`
- `List`

Example: `?r list` `?r list @ChrisJ#8703` `?r list 123123123123`
