#kafka=kafka-2.5.0-src.tgz
cd ~

wget "https://mirror.downloadvn.com/apache/kafka/2.5.0/kafka_2.12-2.5.0.tgz"

curl http://kafka.apache.org/KEYS | gpg --import
wget http://mirrors.viethosting.com/apache/kafka/2.5.0/kafka_2.12-2.5.0.tgz.asc
gpg --verify kafka-2.5.0-src.tgz.asc kafka-2.5.0-src.tgz


sudo mkdir /opt/kafka
sudo tar -xvzf kafka-2.5.0-src.tgz --directory /opt/kafka --strip-components 1

rm -rf kafka-2.5.0-src.tgz kafka-2.5.0-src.tgz.asc


sudo mkdir /var/lib/kafka
sudo mkdir /var/lib/kafka/data


sudo chown -R kafka:nogroup /opt/kafka
sudo chown -R kafka:nogroup /ar/lib/kafka


sudo /opt/kafka/bin/kafka-server-start.sh /opt/kafka/config/server.properties>kafka.log&


#/opt/kafka/bin/kafka-topics.sh --list --zookeeper localhost:2181

#/opt/kafka/bin/kafka-console-producer.sh --broker-list localhost:9092 --topic test

#/opt/kafka/bin/kafka-console-consumer.sh --bootstrap-server localhost:9092 --topic test --from-beginning

#sudo nano /etc/systemd/system/kafka.service

#
[Unit]
Description=High-available, distributed message broker
After=network.target
[Service]
User=kafka
ExecStart=/opt/kafka/bin/kafka-server-start.sh /opt/kafka/config/server.properties
[Install]
WantedBy=multi-user.target
