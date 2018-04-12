# Counter Service Help

The counter service is a service that checks messages to see if they contain a certain phrase, and increments a counter. It is very much unused and I should probably kill it off.

All commands for this service are in the group `Counter`, that is aliased by `C`. This means that instead of typing
`?Counter MakeCounter <name>`, you can type
`?C MakeCounter <name>`.

## Make Counter / New Counter / Make / New

**Requires the user permission to Manage Channels.**

Makes a new counter if it doesn't already exist.

Aliases:
 - `MakeCounter`
 - `NewCounter`
 - `Make`
 - `New`

Parameters:
 - Name: The name of the counter. Must be a single word, meaning no whitespace, or a string enclosed with `"` double quotes. Emojis *should* work, but YMMV.

Example: `?C MakeCounter name`, `?C MakeCounter "this has spaces"`

## Increment / + / Add / Plus / Tally / Tick

Increments the counter, if it exists, by 1.

Aliases:
 - `Increment`
 - `+`
 - `Add`
 - `Plus`
 - `Tally`
 - `Tick`

Parameters:
 - Name: The name of the counter.

 Example: `?C + name`, `?c add "this has spaces"`

## Decrement / - / Subtract / Minus / Untick

See the increment command. Does the same, but subtracts 1 from the value.

Aliases:
 - `Decrement`
 - `-`
 - `Subtract`
 - `Minus`
 - `Untick`

Parameters:
 - Name: The name of the counter.

 Example: `?C - name`, `?c subtract "this has spaces"`


## Delete / Remove

**Requires the user permission to Manage Channels.**

Deletes counters that match the counter text, if any match.

Aliases:
 - `Delete`
 - `Remove`

Parameters:
 - Name: The name of the counter.

Example: `?c delete name`, `?c remove "this has spaces"`

## List Channel / List / Channel

Lists all of the counters in the current channel.

Aliases:
 - `ListChannel`
 - `List`
 - `Channel`

Example: `?c list`

## List Guild / Guild / List Server / Server

Lists all of the counters in the current guild.

Aliases:
 - `ListGuild`
 - `Guild`
 - `ListServer`
 - `Server`

Example: `?c guild`

## Set / Set Counter

Sets the value of the counter.

Parameters:
 - Name: The name of the counter.
 - Value: The integer value to set to the counter.

Aliases:
 - `Set`
 - `SetCounter`

Example: `?c set name 999`, `?c setcounter "spaces lol" -1`
