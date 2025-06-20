﻿using System.Threading.Tasks;

namespace Bookify.Interfaces;

    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
