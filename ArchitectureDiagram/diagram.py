## pip install diagrams
## winget install -e --id Graphviz.Graphviz
## set PATH=C:\Program Files\Graphviz\bin;%PATH%
#
# Architecture of the docker-compose using a chart generated in py

from diagrams import Diagram, Cluster
from diagrams.onprem.inmemory import Redis
from diagrams.onprem.database import Postgresql
from diagrams.onprem.queue import Rabbitmq
from diagrams.programming.language import PHP
from diagrams.onprem.client import Users
from diagrams.onprem.network import Nginx

with Diagram("Composed Docker", show=False):
    users = Users("users")
    
    with Cluster("Front-End"):
        web = Nginx("ngweb")
        php = PHP("php")

    with Cluster("Back-Ends"):
        redis = Redis("cache")
        rabbit = Rabbitmq("rabbit")
        db = Postgresql("db")
    
    users >> web >> php >> rabbit >> db
    php >> redis

