workspace {
    model {
        softwareSystem = softwareSystem "Messenger" {
            database = container "Database" {
                tags "database"
            }
            rabbit = container "RabbitMQ" {
                tags "rabbit"
            }
            messageWriter = container "Message writer"{
                this -> rabbit "Dequeue"
                this -> database "Writes"
            }
            API = container "API" {
                tags "api"
                
                dbContext = component "DB context" {
                    this -> database "Reads"
                }
                messagesRepository = component "Messages repository" {
                    this -> dbContext "Reads"
                }
                messagesService = component "Messages service" {
                    this -> messagesRepository "Reads"
                    this -> rabbit "Queue"
                }
                signalR = component "SignalR" {
                    tags "side"
                    
                    this -> messagesService "Sends"
                }
                messagesController = component "Messages controller" {
                    tags "side"
                    
                    this -> messagesService "Reads"
                }
            }
            WEB = container "WEB" {
                tags "web"
            
                signalR -> this "Sends"
                this -> messagesController "Uses"
                this -> signalR "Sends"
            }
        }
    }
    views {
        styles {
            element api {
                shape hexagon
                background #555555
            }
            element database {
                shape cylinder
                background #30AA30
            }
            element rabbit {
                shape pipe
                background #cc8500
            }
            element web {
                shape webBrowser
            }
            
            element side {
                shape box
                background #707070
            }
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
