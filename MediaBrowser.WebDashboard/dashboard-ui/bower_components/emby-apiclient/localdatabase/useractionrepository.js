define(["idbcore"],function(idb){"use strict";function openUserActionDatabase(){return new Promise(function(resolve,reject){if(databaseInstance)return void resolve(databaseInstance);var request=indexedDB.open(dbName,dbVersion);request.onerror=reject,request.onblocked=reject,request.onupgradeneeded=function(event){var db=request.result;if(event.oldVersion<1&&db.createObjectStore(dbName),event.oldVersion<2){request.transaction.objectStore(dbName).createIndex("Index_ServerId","ServerId")}},request.onsuccess=function(event){var db=event.target.result;db.onversionchange=function(){db.close(),databaseInstance=null},databaseInstance=db,resolve(db)}})}function getActionDb(){return openUserActionDatabase(dbName)}function getByServerId(serverId){return getActionDb().then(function(db){return idb.getObjectsByIndexKeys(db,"Index_ServerId",serverId)})}function getUserAction(key){return getActionDb().then(function(db){return idb.getObjectByKey(db,key)})}function addUserAction(key,val){return getActionDb().then(function(db){return idb.addObject(db,key,val)})}function deleteUserAction(key){return getActionDb().then(function(db){return idb.deleteObject(db,key)})}function clearUserActions(){return getActionDb().then(function(db){return idb.clearObjects(db)})}var databaseInstance,indexedDB=self.indexedDB,dbName="useractions",dbVersion=2;return{getUserAction:getUserAction,addUserAction:addUserAction,deleteUserAction:deleteUserAction,clearUserActions:clearUserActions,getByServerId:getByServerId}});