using Wormhole;

if (args.Length > 0)
{
    string targetFile = args[0];
    await Tiny.run(targetFile);
} else
{
    RegEdit.init();
    Console.ReadKey();
}

