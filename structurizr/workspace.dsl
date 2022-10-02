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
                
                massTransit = component "Mass transit" {
                    this -> messagesRepository "Uses"
                }
                
                signalR = component "SignalR" {
                    this -> WEB "Sends"
                }
                
                messagesService = component "Messages service" {
                    this -> massTransit "Uses"
                    this -> signalR "Uses"
                    this -> messagesRepository "Uses"
                }
                messagesController = component "Messages controller" {
                    this -> messagesService "Uses"
                }
            }
            WEB -> messagesController "Uses"
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
