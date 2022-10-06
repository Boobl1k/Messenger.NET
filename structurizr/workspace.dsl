workspace {
    model {
        softwareSystem = softwareSystem "Messenger" {
            database = container "Database" {
            }
            WEB = container "WEB" {
            }
            API = container "API" {
                tags "software"
                
                dbContext = component "DB context" {
                    this -> database "Uses"
                }
                messagesRepository = component "Messages repository" {
                    this -> dbContext "CRUD"
                }
                messagesService = component "Messages service" {
                    this -> messagesRepository "Uses"
                }
                massTransit = component "Mass transit" {
                    this -> messagesService "Uses"
                }
                signalR = component "SignalR" {
                    this -> WEB "Sends"
                    this -> messagesService "Uses"
                }
                messagesController = component "Messages controller" {
                    this -> messagesService "Uses"
                    this -> massTransit "Uses"
                }
            }
            WEB -> messagesController "Uses"
            WEB -> signalR "Sends"
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
        theme default
    }
}
