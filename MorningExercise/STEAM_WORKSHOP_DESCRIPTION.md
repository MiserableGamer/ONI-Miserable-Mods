[h1]Morning Exercise[/h1]
Adds a Morning Exercise schedule block and Manual Exerciser building, allowing Duplicants to work out and gain Athletics or Morale bonuses.

[h2]Features[/h2]
[list]
[*][b]Morning Exercise Schedule Block[/b] - New schedule block type for exercise time
[*][b]Manual Exerciser Building[/b] - Exercise-only building in the Medicine category (requires Medicine I research)
[*][b]Warm Up Buff[/b] - Regular Duplicants gain Athletics bonus after exercising (configurable, default +3)
[*][b]Bionic Warm Up Buff[/b] - Bionic Duplicants gain Morale bonus instead of Athletics (configurable, default +2)
[*][b]Waiting Chore[/b] - Duplicants wait for equipment instead of going idle when no exercisers are available
[*][b]Configurable Options[/b] - Customize exercise duration, buff duration, and bonus amounts
[*][b]Separate Settings for Dupes and Boops[/b] - Independent configuration for standard Duplicants ("Dupes") and Bionic Duplicants ("Boops")
[*][b]Enable/Disable Exercise[/b] - Toggle exercise on or off for each duplicant type independently
[*][b]Experience Gain[/b] - Optional permanent Athletics attribute experience gain (like Manual Generator)
[*][b]Animation Variant Selection[/b] - Choose between Standard, Variant A, or Variant B Manual Generator animations
[/list]

[h2]How It Works[/h2]
[olist]
[*]Research [b]Medicine I[/b] to unlock the Manual Exerciser
[*]Build [b]Manual Exercisers[/b] in your base (Medicine category)
[*]Add [b]Morning Exercise[/b] blocks to your Duplicants' schedules
[*]During exercise time, Duplicants find an available Manual Exerciser
[*]They exercise for the configured duration (default: 20 seconds)
[*]After completing exercise, they receive the appropriate buff:
[list]
[*][b]Regular Duplicants ("Dupes")[/b]: Warm Up buff (+3 Athletics by default, 6 minutes)
[*][b]Bionic Duplicants ("Boops")[/b]: Bionic Warm Up buff (+2 Morale by default, 12 minutes)
[/list]
[*]If experience gain is enabled, Duplicants also gain permanent Athletics attribute experience (same as Manual Generator)
[*]Duplicants can only exercise once per exercise block
[/olist]

[h2]Configuration[/h2]
Options are organized into sections: [b]Dupes[/b], [b]Boops[/b], [b]Experience Settings[/b], and [b]Visual Settings[/b].

[h3]Dupes (Standard Duplicants)[/h3]
[list]
[*][b]Enable Exercise[/b] (Default: Enabled) - Toggle to allow/prevent standard Duplicants from exercising
[*][b]Exercise Duration (seconds)[/b] (Default: 20)
[list]
[*]Range: 5 to 120 seconds
[/list]
[*][b]Buff Duration (seconds)[/b] (Default: 360)
[list]
[*]Range: 60 to 999 seconds
[*]Default: 6 minutes
[/list]
[*][b]Athletics Bonus[/b] (Default: 3)
[list]
[*]Range: 1 to 10
[/list]
[/list]

[h3]Boops (Bionic Duplicants)[/h3]
[list]
[*][b]Enable Exercise[/b] (Default: Enabled) - Toggle to allow/prevent Bionic Duplicants from exercising
[*][b]Buff Duration (seconds)[/b] (Default: 720)
[list]
[*]Range: 60 to 999 seconds
[*]Default: 12 minutes (longer due to extended bionic cycles)
[/list]
[*][b]Morale Bonus[/b] (Default: 2)
[list]
[*]Range: 1 to 10
[/list]
[/list]

[h3]Experience Settings[/h3]
[list]
[*][b]Enable Experience Gain[/b] (Default: Enabled)
[list]
[*]Grants permanent Athletics attribute experience (like Manual Generator)
[/list]
[*][b]Experience Multiplier[/b] (Default: 1)
[list]
[*]Range: 0 to 10
[*]1 = same rate as Manual Generator (1 exp/second)
[/list]
[/list]

[h3]Visual Settings[/h3]
[list]
[*][b]Animation Variant[/b] (Default: Variant A)
[list]
[*]Options: Standard, Variant A, or Variant B
[*]Requires game restart to apply changes
[/list]
[/list]

[h2]Building Details[/h2]
[b]Manual Exerciser[/b]
[list]
[*]Category: Medicine
[*]Research: Medicine I
[*]Size: 2x2 tiles
[*]Material: All Metals
[*]Function: Exercise-only (does not generate power)
[*]Animation: Selectable variant (Standard, Variant A, or Variant B)
[/list]

[h2]Performance[/h2]
[b]Minimal Performance Impact[/b]
[list]
[*][b]Efficient lookups[/b]: Static registry for fast equipment searches
[*][b]Cached checks[/b]: Equipment availability cached to avoid repeated lookups
[*][b]Optimized updates[/b]: Monitor runs 5 times per second (not every frame)
[*][b]Uses existing systems[/b]: Leverages game's chore and schedule systems
[*][b]One-time initialization[/b]: All setup happens at game startup
[/list]

[h2]Compatibility[/h2]
[list]
[*]All DLCs supported (Base Game, Spaced Out!, The Frosty Planet Pack, The Bionic Booster Pack)
[*]Minimum Build: 700386
[*]API Version: 2
[/list]

[h2]Support & Issues[/h2]
Need help, found a bug, or have a suggestion? We're here to help!

[h3]Community[/h3]
[list]
[*][b]💬 Discord[/b]: [url=https://discord.com/channels/1452947938304200861/1452947939927392398]Join our Discord server[/url] for discussions, questions, and community support
[*][b]📝 GitHub Discussions[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/discussions]Discuss on GitHub[/url] - share ideas, ask questions, or get help with modding
[/list]

[h3]Reporting Issues[/h3]
Found a bug or have a feature request? Please report it on GitHub using our issue templates:
[list]
[*][b]🐛 Bug Reports[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=bug_report.yml]Report a Bug[/url] - Use this for crashes, errors, or unexpected behavior
[*][b]💡 Feature Requests[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=feature_request.yml]Suggest a Feature[/url] - Have an idea for a new feature or improvement?
[*][b]❓ Questions[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=question.yml]Ask a Question[/url] - Need help understanding how something works?
[*][b]📝 Other Issues[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=other.yml]Other Issue[/url] - Something else that doesn't fit the above categories
[/list]

Please mention "Morning Exercise" in your issue title or description.

[h2]Mod Collection[/h2]
This mod is part of the [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156]ONI 200+ Ultimate Mods collection[/url] on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

[h2]Credits[/h2]
[list]
[*]Developed for Oxygen Not Included
[*]Uses PLib by Peter Han
[*]Uses Harmony by pardeike
[/list]

[h2]Future Functionality[/h2]
The following features are planned for future releases:

[b]Custom Animation Variant[/b] - A dedicated custom animation for the Manual Exerciser building, providing a unique visual appearance distinct from the Manual Generator. This would include custom sprites and animations specifically designed for the exercise machine, making it visually distinct while maintaining the same functionality.

[h2]Version History[/h2]
[list]
[*][b]0.1.87[/b]: Initial release
[/list]

Get your Duplicants moving and boost their performance! 🏃‍♂️
