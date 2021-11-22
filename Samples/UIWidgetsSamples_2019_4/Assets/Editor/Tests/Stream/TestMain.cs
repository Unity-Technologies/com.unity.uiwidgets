using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.Editor;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;

namespace Editor.Tests.Stream
{
    public class TestMain : UIWidgetsEditorPanel
    {
        [MenuItem("UIWidgets/Test/Stream")]
        public static void StartTest()
        {
            CreateWindow<TestMain>();
        }
        
        protected override void main()
        {
            ui_.runApp(new TestApp());
        }


        public class TestApp : StatelessWidget
        {
            /**
             * Test Stream.periodic
             */
            private void test1()
            {
                var myStream = Stream<int>.periodic(new TimeSpan(0,0,0,1), t =>
                {
                    Debug.Log("lalalala");
                    return t;
                });

                myStream.listen(val =>
                {
                    Debug.Log("value = " + val);
                });
            }
            /**
             * Test ErrorHandler
             */
            private void test2()
            {
                IEnumerable<int> count()
                {
                    for (int i = 1; i < 5; i++)
                    {
                        if (i == 4)
                        {
                            throw new Exception("Intentional exception");
                        }
                        else
                        {
                            yield return i;
                        }
                    }
                }

                void sumStream(Stream<int> stream, Action<int> onDone)
                {
                    var sum = 0;
                    stream.listen(val =>
                    {
                        sum += val;
                        Debug.Log("sum stream = " + sum);
                    }, 
                        onDone: () =>
                        {
                            onDone.Invoke(sum);
                        },
                        onError: (e, stack) =>
                        {
                            Debug.Log("error at " + stack);
                        });
                }

                var myStream = Stream<int>.fromIterable(count());

                sumStream(myStream, val =>
                {
                    Debug.Log("sum = " + (int)val);
                });
            }
            
            /**
             * Test OnDone/OnData/Stream.fromIterable
             */
            private void test3()
            {
                IEnumerable<int> count()
                {
                    for (int i = 1; i < 5; i++)
                    {
                        yield return i;
                    }
                }

                void sumStream(Stream<int> stream, Action<int> onDone)
                {
                    var sum = 0;
                    stream.listen(val =>
                        {
                            sum += val;
                            Debug.Log("sum stream = " + sum);
                        }, 
                        onDone: () =>
                        {
                            onDone.Invoke(sum);
                        },
                        onError: (e, stack) =>
                        {
                            Debug.Log("error at " + stack);
                        });
                }

                var myStream = Stream<int>.fromIterable(count());

                sumStream(myStream, val =>
                {
                    Debug.Log("sum = " + (int)val);
                });
            }

            /**
             * Test streamTransform Where
             */
            private void test4()
            {
                Stream<int> numbers = Stream<int>.fromIterable(new List<int> {0, 1, 2, 3}).where(n => n % 2 == 0);
                numbers.listen(n =>
                {
                    Debug.Log("num = " + n);
                });
            }

            /**
             * Test Stream.take
             */
            private void test5()
            {
                Stream<int> numbers = Stream<int>.periodic(new TimeSpan(0, 0, 0, 1), t => t).take(3);
                numbers.listen(n =>
                {
                    Debug.Log("num = " + n);
                }, onDone: () =>
                {
                    Debug.Log("periodic finished");
                });
            }

            /**
             * Test Stream.asBroadcastStream
             */
            private void test6()
            {
                Stream<int> numbers = Stream<int>.periodic(new TimeSpan(0, 0, 0, 1), t => t).asBroadcastStream().take(10);

                var subscription1 = numbers.listen((data) =>
                {
                    Debug.Log("Sub1: " + data);
                });
                
                var subscription2 = numbers.listen((data) =>
                {
                    Debug.Log("Sub2: " + data);
                    if (data == 3)
                    {
                        subscription1.cancel();
                    }
                });
            }
            
            /**
             * Test listen( ..., cancelOnError = true)
             */
            private void test7()
            {
                Stream<int> numbers = Stream<int>.periodic(new TimeSpan(0, 0, 0, 1), t =>
                {
                    if (t == 2)
                    {
                        throw new Exception("LaLaLa");
                    }
                    
                    return t;
                }).take(5);
                void sumStream(Stream<int> stream, Action<int> onDone)
                {
                    var sum = 0;
                    stream.listen(val =>
                        {
                            sum += val;
                            Debug.Log("sum stream = " + sum);
                        }, 
                        onDone: () =>
                        {
                            onDone.Invoke(sum);
                        },
                        onError: (e, stack) =>
                        {
                            Debug.Log("error at " + stack);
                        },
                        cancelOnError: true);
                }

                sumStream(numbers, val =>
                {
                    Debug.Log("sum = " + (int)val);
                });
            }

            /**
             * Test subscription.pause/resume/cancel
             */
            private void test8()
            {
                Stream<int> numbers = Stream<int>.periodic(new TimeSpan(0, 0, 0, 1), t => t).take(3);
                var subscription = numbers.listen(n =>
                {
                    Debug.Log("num = " + n);
                }, onDone: () =>
                {
                    Debug.Log("periodic finished");
                });

                Future.delayed(new TimeSpan(0, 0, 0, 0, 1200), () =>
                {
                    Debug.Log("pause >>>>");
                    subscription.pause();
                    return FutureOr.nil;
                }).then(v =>
                {
                    Future.delayed(new TimeSpan(0, 0, 0, 5), () =>
                    {
                        Debug.Log("resume >>>>");
                        subscription.resume();
                        return FutureOr.nil;
                    }).then(v2 =>
                    {
                        Future.delayed(new TimeSpan(0, 0, 0, 1), () =>
                        {
                            Debug.Log("cancel >>>>");
                            subscription.cancel();
                            return FutureOr.nil;
                        });
                    });
                });
            }

            /**
             * Test Stream.map, distinct
             */
            private void test9()
            {
                string convert(int number)
                {
                    return "string " + number;
                }

                bool stringEqual(string s1, string s2)
                {
                    return s1 == s2;
                }
                
                Stream<string> numbers = Stream<int>.fromIterable(new List<int> {0, 1, 2, 2, 3, 4, 5, 5}).map(convert).distinct(stringEqual);
                numbers.listen(val =>
                {
                    Debug.Log("val = " + val);
                });
            }

            private void test10()
            {
                Stream<int> numbers = Stream<int>.fromIterable(new List<int> {0, 1, 2, 2, 3, 4, 5, 5});
                var transformer = StreamTransformer<int, string>.fromHandlers(handleData: (val, sink) =>
                {
                    sink.add("My number is " + val);
                });

                numbers.transform(transformer).listen(val =>
                {
                    Debug.Log("val = " + val);
                });
            }

            /**
             * Test StreamController
             */
            private void test11()
            {
                StreamController<float> controller = StreamController<float>.create();
                Stream<float> stream = controller.stream;

                var value = 1f;
                var timer = Timer.periodic(new TimeSpan(0, 0, 0, 1), (v) =>
                {
                    value = value * 1.2f;
                    controller.add(value);
                    return null;
                });
                
                stream.listen((val) =>
                {
                    if (val >= 2)
                    {
                        timer.cancel();
                    }
                    Debug.Log("value = " + val);
                });
            }

            /**
             * Test Stream.fromFuture
             */
            private void test12()
            {
                Future<string> getData()
                {
                    return Future<string>.delayed(new TimeSpan(0, 0, 0, 1), () =>
                    {
                        return "My String from Future";
                    }).to<string>();
                }
                
                var stream = Stream<string>.fromFuture(getData());
                stream.listen(val =>
                {
                    Debug.Log("val = " + val);
                });
            }

            /**
             * Test Stream.multi
             */
            private void test13()
            {
                var log = new List<string>();
                var index = 1;

                var multi = StreamMultiUtils<List<int>>.multi(c =>
                {
                    var id = index++;
                    log.Add($"{id}");
                    for (var i = 0; i < id + 1; i++)
                    {
                        c.add(new List<int>{id, i});
                    }

                    c.close();
                });

                void logList(List<int> l)
                {
                    log.Add($"{l.first()}-{l.last()}");
                }

                Future.wait<object>(new List<Future> {multi.forEach(logList), multi.forEach(logList)}).whenComplete(
                    () =>
                    {
                        foreach (var str in log)
                        {
                            Debug.Log(str);
                        }
                    }
                );
            }
            
            public override Widget build(BuildContext context)
            {
                test13();
                return new Container();
            }
        }
    }
}