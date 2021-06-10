#coding=utf-8
import vk
import sys

ids = str(sys.argv[1])
token = ""  # Сервисный ключ доступа
session = vk.Session(access_token=token)
api = vk.API(session, v='11.9.1')
person = api.users.get(user_ids=ids, fields='about, education, home_town, exports, interests, bdate')
print(person)
#'pavlovsemen'