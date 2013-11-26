package mina_test;

import java.net.InetSocketAddress;
import org.apache.mina.core.filterchain.DefaultIoFilterChainBuilder;
import org.apache.mina.filter.codec.ProtocolCodecFilter;
import org.apache.mina.filter.codec.textline.TextLineCodecFactory;
import org.apache.mina.filter.logging.LoggingFilter;
import org.apache.mina.filter.logging.MdcInjectionFilter;
import org.apache.mina.transport.socket.nio.NioSocketAcceptor;

/**
 * (<b>Entry point</b>) Chat server
 * 
 * @author The Apache MINA Project (dev@mina.apache.org)
 * @version $Rev$, $Date$
 */
public class Main {
	/** Choose your favorite port number. */
	private static final int PORT = 1234;

	public static void main(String[] args) throws Exception {
		// 新建服务器socket
		NioSocketAcceptor acceptor = new NioSocketAcceptor();
		
		acceptor.getSessionConfig().setTcpNoDelay(true);

		// 设置收到消息的过滤链
		DefaultIoFilterChainBuilder chain = acceptor.getFilterChain();
		// MdcInjectionFilter mdcInjectionFilter = new MdcInjectionFilter();
		// chain.addLast("mdc", mdcInjectionFilter);
		//
		
		chain.addLast("codec", new ProtocolCodecFilter(
				new TextLineCodecFactory()));
		//
		// chain.addLast("logger", new LoggingFilter());

		// 设置消息的处理handler
		acceptor.setHandler(new ChatProtocolHandler());

		// Bind
		acceptor.bind(new InetSocketAddress(PORT));

		System.out.println("Listening on port " + PORT);
	}

}
