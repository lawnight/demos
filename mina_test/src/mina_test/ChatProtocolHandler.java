package mina_test;

import java.util.ArrayList;
import java.util.Collections;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

import org.apache.mina.core.service.IoHandler;
import org.apache.mina.core.service.IoHandlerAdapter;
import org.apache.mina.core.session.IoSession;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * {@link IoHandler} implementation of a simple chat server protocol.
 * 
 * @author The Apache MINA Project (dev@mina.apache.org)
 * @version $Rev$, $Date$
 */
public class ChatProtocolHandler extends IoHandlerAdapter {
	private final Logger logger = LoggerFactory.getLogger(getClass());

	private final Set<IoSession> sessions = Collections
			.synchronizedSet(new HashSet<IoSession>());

	private final Set<String> users = Collections
			.synchronizedSet(new HashSet<String>());

	private List _client = new ArrayList();

	private static Object lock = new Object();

	@Override
	public void exceptionCaught(IoSession session, Throwable cause) {
		System.out.print("unexpert error");
		System.out.print(cause);
		session.close(true);
	}

	@Override
	public void sessionOpened(IoSession session) {
		try {
			synchronized (lock) {
				_client.add(session);

				if (_client.size() >= count) {
					borcast();
				}
			}
			// System.out.format("get client  %d \n",_client.size());
		} catch (Exception e) {
			System.out.print("sessionOpened error");
			System.out.print(e);
		}
	}

	@Override
	public void messageReceived(IoSession session, Object message) {
		try {

		} catch (Exception e) {
			System.out.print("illegal argument");
			logger.debug("Illegal argument", e);
		}
	}

	long star = 0;
	long end = 0;

	private void borcast() {
		star = System.currentTimeMillis();
		for (int i = 0; i < _client.size(); i++) {
			IoSession item = (IoSession) _client.get(i);
			item.write("hello");
		}

	}

	int count = 5000;
	int _count = 0;

	@Override
	public void messageSent(IoSession session, Object message) throws Exception {
		_count++;
		if (_count >= count) {
			end = System.currentTimeMillis();
			System.out.print("cost timer");
			System.out.print(end - star);
		}

	}

	@Override
	public void sessionClosed(IoSession session) throws Exception {
		System.out.print("count some");
		_client.remove(session);
	}
}