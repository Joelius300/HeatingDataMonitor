using System;
using System.Threading.Tasks;
using CliFx;

namespace HeatingDataMonitor.Backup
{
    /* PostgreSQL has backup capabilities of its own, why not use those?
     * - It's not only for backup purposes but also to have access to
     *   large amounts of heating-data without having to query the api or the db
     *   (this will be useful for the machine learning)
     * - I get the opportunity to learn about scheduling tasks, mounting network drives
     *   and other backup related stuff
     * - The existing backup tools seem to worry about much more than I need to worry about
     * - Setting up those backup tools to work exactly like I want them to
     *   seems to be quite some effort which I can save myself
     * - I wanted to finally write a usable CLI-tool with the help of CliFx
     */
    class Program
    {
        public static async Task<int> Main() =>
            await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .Build()
                .RunAsync();
    }
}
