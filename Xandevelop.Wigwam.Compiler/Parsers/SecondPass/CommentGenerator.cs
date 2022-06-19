namespace Xandevelop.Wigwam.Compiler.Parsers.SecondPass
{
    // As we patch statements up, any with a null description MAY get an auto generated description.
    // This is based on things like "did we reference a control in the target", which we don't really know until after first pass.
    // E.g.
    // test | do something
    //  click | ${Login Button}
    //
    // control | Login Button | id=login | friendly name = Login button on the home screen
    //
    // Here, the click is probably valid on first pass, but we don't know for sure until after we process the control.
    // Since the click has no comment, there are 2 second pass things to do with it:
    // 1. Generate an auto comment (because we know we're clicking on a known control) - this can be something like "Click on Login button on the home screen"
    // 2. Change the target to a pointer to the control, rather than a variable string
    //      (note: pointer only, don't dereference it in case it's going to be compiled to C# or similar which wants to keep a reference for easier maintainance.  Only SIDE wants the dereference to happen)
    class CommentGenerator
    {
    }
}
