INSERT INTO dbo."AspNetRoles"("Id", "Name", "Description", "Discriminator") VALUES ('a76b488c-338c-4da6-9f3d-a5754360ac97', 'Admin', NULL, 'ApplicationRole');


INSERT INTO dbo."AspNetUsers"(
            "Id", "Email", "EmailConfirmed", "PasswordHash", "SecurityStamp", 
            "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEndDateUtc", 
            "LockoutEnabled", "AccessFailedCount", "UserName")
    VALUES ('786f43d1-86cb-41e2-ac4d-c9bdb825e664', 'admin@edpl.com', False, 'AJVizORTxZB5BpVUmHv4JNohQquWLxd3BqZj6a5qXLhFS9JrYpcdSu6eNgyahwiAEA==', '22749dfc-785c-4119-b748-5351fc735031', 
            NULL, False, False, NULL, 
            True, 0, 'admin@edpl.com');
			
			
INSERT INTO dbo."AspNetUserRoles"("UserId", "RoleId") VALUES ('786f43d1-86cb-41e2-ac4d-c9bdb825e664', 'a76b488c-338c-4da6-9f3d-a5754360ac97');