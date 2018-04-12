---
title: Reminder Service Help
categories: [guide]
layout: topic
---

# Reminder Service help

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
