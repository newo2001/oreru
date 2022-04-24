namespace BPM.Tests

open System
open Xunit
open FParsec
open BPM.Storyboard
open BPM.StoryboardParser
open BPM.Common

module StoryboardTests =
    let parse p s =
        match run p s with
        | Success(result, _, _) -> result
        | Failure(err, _, _) -> raise (Exception(err))
    
    type ``Parsing a layer should return correct layer`` () =
        static member LayerSamples
            with get(): obj[] seq = [
                [| "Background"; Background |]
                [| "Foreground"; Foreground |]
                [| "Fail"; Fail |]
                [| "Pass"; Pass |]
            ]

        [<Theory>]
        [<MemberData("LayerSamples")>]
        member verify.``string is parsed correctly`` str expected =
            let actual = parse layer (str + ",")
            Assert.Equal(expected, actual)
            
        [<Theory>]
        [<MemberData("LayerSamples")>]
        member verify.``quoted string is parsed correctly`` str expected =
            let actual = parse layer $"\"{str}\","
            Assert.Equal(expected, actual)
    
    [<Fact>]
    let ``Parsing a valid sprite should return a sprite`` () =
        let command = "Sprite,Background,Centre,\"test.jpg\",320,240"
        let expected = Sprite {
            layer = Background
            origin = Center
            filePath = "test.jpg"
            x = 320; y = 240
        }
        
        let actual = parse parseSprite command
        Assert.Equal(expected, actual)
    
    [<Fact>]
    let ``Parsing a valid animation should return an animation`` () =
        let command = "Animation,Pass,CentreLeft,\"images/test.jpg\",69,420,42,100,LoopOnce"
        let expected = Animation {
            layer = Pass
            origin = CenterLeft
            filePath = "images/test.jpg"
            x = 69; y = 420
            frames = 42; frameDelay = 100
            loop = Once
        }
        
        let actual = parse parseAnimation command
        Assert.Equal(expected, actual)
    
    [<Fact>]
    let ``Parsing an an invalid command type throws an exception`` () =
        let command = "_G"
        Assert.Throws<Exception>(fun () -> parse parseCommand command |> ignore)
    
    [<Fact>]
    let ``Parsing a valid fade command should return correct command`` () =
        let command = "_F,0,1000,3000,0,0.5"
        let expected = FadeCommand {
            easing = Linear
            startTime = TimeSpan.FromMilliseconds(1000)
            endTime = TimeSpan.FromMilliseconds(3000)
            startOpacity = 0
            endOpacity = 0.5
        }
        
        let actual = parse parseCommand command
        Assert.Equal(expected, actual)
    
    [<Fact>]
    let ``Parsing a valid move command should return correct command`` () =
        let command = "_M,7,0,6000,-110,-100,740,580"
        let expected = MoveCommand {
            easing = CubicOut
            startTime = TimeSpan.Zero
            endTime = TimeSpan.FromMilliseconds(6000)
            startX = -110; startY = -100
            endX = 740; endY = 580
        }
        
        let actual = parse parseCommand command
        Assert.Equal(expected, actual)
        
    [<Fact>]
    let ``Parsing a valid horizontal move command should return correct command`` () =
        let command = "_MX,1,1500,6000,-110,740"
        let expected = MoveXCommand {
            easing = EaseOut
            startTime = TimeSpan.FromMilliseconds(1500)
            endTime = TimeSpan.FromMilliseconds(6000)
            startX = -110; endX = 740
        }
        
        let actual = parse parseCommand command
        Assert.Equal(expected, actual)
    
    [<Fact>]
    let ``Parsing a valid vertical move command should return correct command`` () =
        let command = " MY,2,0,0,500,0"
        let expected = MoveYCommand {
            easing = EaseIn
            startTime = TimeSpan.Zero
            endTime = TimeSpan.Zero
            startY = 500; endY = 0
        }
        
        let actual = parse parseCommand command
        Assert.Equal(expected, actual)
    
    [<Fact>]
    let ``Parsing a valid scale command should return correct command`` () =
        let command = "_S,0,36500,37000,0,5.5"
        let expected = ScaleCommand {
            easing = Linear
            startTime = TimeSpan.FromMilliseconds(36500)
            endTime = TimeSpan.FromMilliseconds(37000)
            startScale = 0; endScale = 5.5
        }
        
        let actual = parse parseCommand command
        Assert.Equal(expected, actual)
    
    [<Fact>]
    let ``Parsing a valid vector scale command should return correct command`` () =
        let command = " V,34,36500,37000,1,2,3,0.5"
        let expected = VectorScaleCommand {
            easing = BounceInOut
            startTime = TimeSpan.FromMilliseconds(36500)
            endTime = TimeSpan.FromMilliseconds(37000)
            startScaleX = 1; startScaleY = 2
            endScaleX = 3; endScaleY = 0.5
        }
        
        let actual = parse parseCommand command
        Assert.Equal(expected, actual)
    
    [<Fact>]
    let ``Parsing a valid rotation command should return correct command`` () =
        let command = "_R,0,47210,47810,-0.785,0.785"
        let expected = RotateCommand {
            easing = Linear
            startTime = TimeSpan.FromMilliseconds(47210)
            endTime = TimeSpan.FromMilliseconds(47810)
            startAngle = -0.785
            endAngle = 0.785
        }
        
        let actual = parse parseCommand command
        Assert.Equal(expected, actual)
    
    [<Fact>]
    let ``Parsing a valid color command should return correct command`` () =
        let command = "_C,0,58810,59810,0,80,0,255,128,255"
        let expected = ColorCommand {
            easing = Linear
            startTime = TimeSpan.FromMilliseconds(58810)
            endTime = TimeSpan.FromMilliseconds(59810)
            startColor = (0uy, 80uy, 0uy)
            endColor = (255uy, 128uy, 255uy)
        }
        
        let actual = parse parseCommand command
        Assert.Equal(expected, actual)
        
    [<Fact>]
    let ``Parsing a valid horizontal flip command should return correct command`` () =
        let command = "_P,0,500,700,H"
        let expected = FlipCommand {
            easing = Linear
            startTime = TimeSpan.FromMilliseconds(500)
            endTime = TimeSpan.FromMilliseconds(700)
            axis = Horizontal
        }
        
        let actual = parse parseCommand command
        Assert.Equal(expected, actual)
    
    [<Fact>]
    let ``Parsing a valid vertical flip command should return correct command`` () =
        let command = "_P,0,500,700,V"
        let expected = FlipCommand {
            easing = Linear
            startTime = TimeSpan.FromMilliseconds(500)
            endTime = TimeSpan.FromMilliseconds(700)
            axis = Vertical
        }
        
        let actual = parse parseCommand command
        Assert.Equal(expected, actual)
    
    [<Fact>]
    let ``Parsing a valid color blending command should return correct command`` () =
        let command = "_P,0,500,700,A"
        let expected = AdditiveBlendCommand {
            easing = Linear
            startTime = TimeSpan.FromMilliseconds(500)
            endTime = TimeSpan.FromMilliseconds(700)
        }
        
        let actual = parse parseCommand command
        Assert.Equal(expected, actual)
        
    [<Fact>]
    let ``Parsing a valid loop command should return correct command`` () =
        let command = "_L,900,7"
        let expected = LoopCommand {
            startTime = TimeSpan.FromMilliseconds(900)
            iterations = 7
        }
        
        let actual = parse parseCommand command
        Assert.Equal(expected, actual)
    
    [<Fact>]
    let ``Parsing a valid effect should return correct effect`` () =
        let effect = "Sprite,Background,Centre,\"test.jpg\",320,240\n\
                      _F,0,1000,3000,0,0.5\n\
                      _P,0,500,700,A"
        let expected = {
            image = Sprite {
                layer = Background
                origin = Center
                filePath = "test.jpg"
                x = 320; y = 240
            }; commands = [
                FadeCommand {
                    easing = Linear
                    startTime = TimeSpan.FromMilliseconds(1000)
                    endTime = TimeSpan.FromMilliseconds(3000)
                    startOpacity = 0
                    endOpacity = 0.5
                }; AdditiveBlendCommand {
                    easing = Linear
                    startTime = TimeSpan.FromMilliseconds(500)
                    endTime = TimeSpan.FromMilliseconds(700)
                }
            ]
        }
        
        let actual = parse parseEffect effect
        
        Assert.Equal(expected, actual)
    
    [<Fact>]
    let ``Parsing a valid storyboard should return correct storyboard`` () =
        let storyboard = "Sprite,Background,Centre,\"test.jpg\",320,240\n\
                          _F,0,1000,3000,0,0.5\n\
                          Animation,Pass,CentreLeft,\"images/test.jpg\",69,420,42,100,LoopOnce\n\
                          _P,0,500,700,V"
        let expected = {
            effects = [
                {
                    image = Sprite {
                        layer = Background
                        origin = Center
                        filePath = "test.jpg"
                        x = 320; y = 240
                    }; commands = [
                        FadeCommand {
                            easing = Linear
                            startTime = TimeSpan.FromMilliseconds(1000)
                            endTime = TimeSpan.FromMilliseconds(3000)
                            startOpacity = 0
                            endOpacity = 0.5
                        }
                    ]
                }; {
                    image = Animation {
                        layer = Pass
                        origin = CenterLeft
                        filePath = "images/test.jpg"
                        x = 69; y = 420
                        frames = 42; frameDelay = 100
                        loop = Once
                    }; commands = [
                        FlipCommand {
                            easing = Linear
                            startTime = TimeSpan.FromMilliseconds(500)
                            endTime = TimeSpan.FromMilliseconds(700)
                            axis = Vertical
                        }
                    ]
                }
            ]
        }
        
        let actual = ParseStoryboard storyboard
        
        Assert.Equal(expected, actual)