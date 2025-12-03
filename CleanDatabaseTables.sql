DELETE FROM events.mt_doc_deadletterevent;
DELETE FROM events.mt_event_progression;
DELETE FROM events.mt_events;
DELETE FROM events.mt_streams;
DELETE FROM readmodels.mt_doc_categoryreadmodel;
DELETE FROM readmodels.mt_doc_employeereadmodel;
DELETE FROM readmodels.mt_doc_organizationreadmodel;
DELETE FROM readmodels.mt_doc_questionnaireassignmentreadmodel;
DELETE FROM readmodels.mt_doc_questionnaireresponsereadmodel;
DELETE FROM readmodels.mt_doc_questionnairetemplatereadmodel;
DELETE FROM readmodels.mt_doc_reviewchangelogreadmodel;
DELETE FROM readmodels.mt_doc_uitranslation;


powershell.exe -ExecutionPolicy Bypass -File .\insert-test-data.ps1 --ClientSecret ""
or
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Unrestricted