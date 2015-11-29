using System;
using System.Globalization;
using System.IO;
using System.Web;

namespace Syborg.Razor
{
    public class HelperResult : IHtmlString
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HelperResult"/> class. 
        /// </summary>
        /// <param name="writerAction">
        /// The writer Action.
        /// </param>
        public HelperResult (Action<TextWriter> writerAction)
        {
            if (writerAction == null)
                throw new ArgumentNullException ("writerAction");
            this.writerAction = writerAction;
        }

        /// <summary>
        /// This type/member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ToHtmlString ()
        {
            return this.ToString ();
        }

        /// <summary>
        /// This type/member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString ()
        {
            using (StringWriter stringWriter = new StringWriter (CultureInfo.InvariantCulture))
            {
                this.writerAction (stringWriter);
                return stringWriter.ToString ();
            }
        }

        /// <summary>
        /// This type/member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        public void WriteTo (TextWriter writer)
        {
            this.writerAction (writer);
        }

        private readonly Action<TextWriter> writerAction;
    }
}