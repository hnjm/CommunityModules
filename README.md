# CommunityModules
## Project description
This solution contains several generic modules that are often involved in web communities.
All modules have their own razor class library.
All the modules use the latest version of bootstrap.

## How to run the project
You must have a valid WSL 2 distribution installed on your machine to run the GPG encryption.
https://learn.microsoft.com/en-us/windows/wsl/install

Install Ubuntu-22.04 from the Microsoft store and then install the following package:
sudo apt install libgpgme11

When you run the unit test inside Visual Studio, make sure you are running them using the WSL system.
Also run and debug the code inside WSL.

## GPG Encryption:
For windows download this software: https://www.gpg4win.org/

## Modules description
### Branding module.
The branding module contains references to the custom bootstrap styles.
Square logo and rectangle logo.
All these elements are partials views that can be redefined to customize the branding.

### Account module
User account management is located in this module.
- Login
- Register
- Update account

The login mechanism is based on Indentity, with some modifications and a custom UI.
The user can enable GPG 2FA.

### User's public profile management module
This module allows the user to edit his own public profile.

### Staff public profile management module
This module allows the staffs to edit the public profile of a user.
It can also be use to give special titles to the user.

### View public profile
Contains a page to view the public profile.

### User account management.
This section is used to delete and perma ban users by the global moderator and admin.
It is also possible to create user accounts and disable user registration from this area.

### Forum Administation
With this module, the administrator can easily access the forum configuration.
- Create, update and delete forums.
- Create announcements

### Personal messages system
This module allows the users to sends PMs to each others.

### Forum
This module enables the user to read forum messages, and post.
Works with BB code.

Mods have access to special buttons that allow them to ban users, edit messages, or delete them.

Integrates with the PM system.
