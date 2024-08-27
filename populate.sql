INSERT INTO
    "Customers" ("Id", "Cpf", "Name")
VALUES
    (1, "01234567890", "admin");

INSERT INTO
    "Accounts" (
        "Id",
        "AccountNumber",
        "Active",
        "CreatedAt",
        "OwnerId",
        "PasswordHash",
        "RefreshToken",
        "RefreshTokenExpiration",
        "Role"
    )
VALUES
    (
        1,
        1724731066,
        1,
        "2024-06-05T12:00:00",
        1,
        "IKXqabRp2BpGqZv8IrWxGA==;LGEbV98LH8Mx0h92W6Fzmo2UfoMU6euVsK9rbVPQd9g=",
        NULL,
        NULL,
        "Admin"
    );

-- password plain text = admin