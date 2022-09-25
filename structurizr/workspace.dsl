workspace {
    model {
        softwareSystem = softwareSystem "Messenger" {
            database = container "Database" {
                users = component "Users"
                messages = component "Messages"
                dialogues = component "Dialogues" {
                    this -> messages "messages[]"
                }
                users_dialogues = component "users_dialogues" {
                    this -> users "user"
                    this -> dialogues "dialogue"
                }
            }
            API = container "API" {
                tags "software"
                
                dbContext = component "DB context" {
                    this -> database "Uses"
                }
                identityService = component "Identity service" {
                    this -> dbContext "CRUD"
                }
                dialoguesService = component "Dialogues service" {
                    this -> dbContext "CRUD"
                    this -> identityService "Uses"
                }
                messagesService = component "Messages service" {
                    this -> dbContext "CRUD"
                    this -> dialoguesService "Uses"
                    this -> identityService "Uses"
                }
                
                authController = component "Auth controller" {
                    this -> identityService "Uses"
                }
                messagesController = component "Messages controller" {
                    this -> messagesService "Uses"
                }
                dialoguesController = component "Dialogues controller" {
                    this -> dialoguesService "Uses"
                }
            }
            WEB = container "WEB" {
                this -> API "Uses"
            }
        }
    }
    views {
        styles {
           /* element payment {
                background #111111
            }
            element software {
                background #555555
            }*/
        }
        /*systemContext softwareSystem {
            include *
        }*/
        container softwareSystem {
            include *
        }
        component API {
            include *
        }
        component Database {
            include *
        }
        theme default
    }
}
