chmod +x ./BattleServer.x86_64 

./BattleServer.x86_64 \
--host ${HOST_ADDRESS}:${HOST_PORT} \
--listen ${LISTEN_ADDRESS}:${LISTEN_PORT} \
--id ${BATTLE_ID} \
--zkroot ${ZK_ROOT} \
--zklogin ${ZK_LOGIN_ROOT} \
--exconfig ${ZK_EXCEL_ROOT} \
--maxplayer ${MAX_PLAYER} \
--zkmatch ${ZK_MATCH_ROOT} \
--zk ${ZK_SERVER} \
--kafka ${KAFKA_SERVER}\
--map ${MAP_ID}
