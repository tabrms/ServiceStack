﻿using NUnit.Framework;
using ServiceStack.Templates;
using ServiceStack.Text;

#if NETCORE
using Microsoft.Extensions.Primitives;
#endif

namespace ServiceStack.WebHost.Endpoints.Tests.TemplateTests
{
    public class TemplateCallExpressionTests
    {
        [Test]
        public void Does_parse_call_expression()
        {
            JsCallExpression expr;

            "a".ToStringSegment().ParseJsCallExpression(out expr);
            Assert.That(expr, Is.EqualTo(new JsCallExpression(new JsIdentifier("a"))));
            "a()".ToStringSegment().ParseJsCallExpression(out expr);
            Assert.That(expr, Is.EqualTo(new JsCallExpression(new JsIdentifier("a"))));
        }

        [Test]
        public void Can_parse_expressions_with_methods()
        {
            JsToken token;
            
            "mod(it,3) != 0".ParseJsExpression(out token);
            Assert.That(token, Is.EqualTo(
                new JsBinaryExpression(
                    new JsCallExpression(
                        new JsIdentifier("mod"),
                        new JsIdentifier("it"),
                        new JsLiteral(3)
                    ),
                    JsNotEquals.Operator, 
                    new JsLiteral(0)
                )
            ));

            "{ a: add(it % add(it + 1, 1 | it)) }".ParseJsExpression(out token);
            Assert.That(token, Is.EqualTo(new JsObjectExpression(
                new JsProperty(
                    new JsIdentifier("a"),
                    new JsCallExpression(
                        new JsIdentifier("add"),
                        new JsBinaryExpression(
                            new JsIdentifier("it"),
                            JsMod.Operator,
                            new JsCallExpression(
                                new JsIdentifier("add"),
                                new JsBinaryExpression(
                                    new JsIdentifier("it"),
                                    JsAddition.Operator,
                                    new JsLiteral(1)
                                ),
                                new JsBinaryExpression(
                                    new JsLiteral(1),
                                    JsBitwiseOr.Operator,
                                    new JsIdentifier("it")
                                )
                            )
                        )
                    )
                )
            )));
        }

        [Test]
        public void Does_evaluate_nested_call_expressions()
        {
            var context = new TemplateContext {
                Args = {
                    ["it"] = 10
                }
            }.Init();
            
            Assert.That(context.EvaluateTemplate("{{ { a: add(it % 3,1) * 2, b: 2 * 3 + incr(4 + decr(5)) } | values | sum | currency }}"), 
                Is.EqualTo("$19.00"));
        }
    }
}