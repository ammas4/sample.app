using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSample.Domain.Models.Constants
{
    public class ControllerUrls
    {
        public const string AskUserTextPath = "ussd/askusertext.ru";

        /// <summary>
        /// Путь для приёма ответа на пуш
        /// </summary>
        public const string McPushAnswerPath = "mc/push/answer";

        /// <summary>
        /// Путь для приёма ответа на пуш с пином
        /// </summary>
        public const string McPushPinAnswerPath = "mc/push-pin/answer";
        /// <summary>
        /// Путь для приёма ответа на пуш
        /// </summary>
        public const string DstkPushAnswerPath = "dstk/push/answer";

        /// <summary>
        /// Путь для приёма ответа на пуш с пином
        /// </summary>
        public const string DstkPushPinAnswerPath = "dstk/push-pin/answer";
        public const string HheRequestUrl = "/hhe/request";
        public const string HheEnrichmentUrl = "/hhe/enrichment";
        public const string UssdUserAnswerRouteName = "UssdUserAnswer";
    }
}
